using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using knesset_app.DBEntities;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data.Entity;



namespace knesset_app
{

    public class ProtocolsComparer : IEqualityComparer<Protocol>
    {
        public bool Equals(Protocol x, Protocol y)
        {
            if (x.pr_number == y.pr_number)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Protocol obj)
        {
            return obj.pr_number.GetHashCode();
        }
    }


    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : Window
    {
        int MAX_INVITATIONS = 1000;

        int MAX_PRESENCE = 1000;

        KnessetContext context = new KnessetContext();

        public SearchWindow()
        {
            InitializeComponent();

            PopulateComboboxes();
        }

        private void PopulateComboboxes()
        {
            cbProtocolTitle.ItemsSource = context.Protocols.Take(10).ToList();
            cbProtocolTitle.DisplayMemberPath = "pr_title";
            cbProtocolTitle.SelectedValuePath = "pr_number";

            cbProtocolCommitte.ItemsSource = context.Committees.Take(10).ToList();
            cbProtocolCommitte.DisplayMemberPath = "c_name";
            cbProtocolCommitte.SelectedValuePath = "c_name";

            Dictionary<string, object> invitedPpl = new Dictionary<string, object>();
            List<Invitation> lstInvitations = context.Invitations.Take(MAX_INVITATIONS).ToList();
            foreach (Invitation invitation in lstInvitations)
            {
                if (invitedPpl.ContainsKey(invitation.pn_name) == false)
                {
                    object val = invitation.pn_name;
                    invitedPpl.Add(invitation.pn_name, val);
                }
            }
            cbInvited.ItemsSource = invitedPpl;

            Dictionary<string, object> presencedPpl = new Dictionary<string, object>();
            List<Presence> lstPresences = context.Persence.Take(MAX_PRESENCE).ToList();
            foreach (Presence presence in lstPresences)
            {
                if (presencedPpl.ContainsKey(presence.pn_name) == false)
                {
                    object val = presence.pn_name;
                    presencedPpl.Add(presence.pn_name, val);
                }
            }
            cbPersence.ItemsSource = presencedPpl;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get TabControl reference.
            TabControl item = sender as TabControl;
            // ... Set Title to selected tab header.
            var selected = item.SelectedItem as TabItem;
            this.Title = selected.Header.ToString();
            TabItem ti = (TabItem)item.SelectedItem;

            if (ti.Name == "tiSearchCommitties")
            {

            }
        }



        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AutoCompleteBox acb = (AutoCompleteBox)sender;

            // In these sample scenarios, the Tag is the name of the content 
            // presenter to use to display the value.
            string contentPresenterName = (string)acb.Tag;
            ContentPresenter cp = FindName(contentPresenterName) as ContentPresenter;
            if (cp != null)
            {
                cp.Content = acb.SelectedItem;
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {


            List<Protocol> protocols = new List<Protocol>();
            //getting the date fields 
            DateTime? fromDate = dpFromDate.SelectedDate;
            DateTime? toDate = dpToDate.SelectedDate;
            //protocol title field  - getting the number of the selected protocol by title   
            int selectedProtocolNumber = 0;
            if (cbProtocolTitle.SelectedValue != null)
            {
                if (cbProtocolTitle.SelectedValue is int)
                {
                    selectedProtocolNumber = (int)cbProtocolTitle.SelectedValue;
                }
            }

            //commettie field 
            string selectedProcotocolCommitte = string.Empty;
            if (cbProtocolCommitte.SelectedValue != null && string.IsNullOrWhiteSpace(cbProtocolCommitte.SelectedValue.ToString()) == false)
            {
                selectedProcotocolCommitte = cbProtocolCommitte.SelectedValue.ToString();
            }

            List<string> lstInvitedNames = new List<string>();
            if (cbInvited.SelectedItems != null && cbInvited.SelectedItems.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in cbInvited.SelectedItems)
                {
                    if (lstInvitedNames.Contains(pair.Key) == false)
                    {
                        lstInvitedNames.Add(pair.Key);
                    }
                }
            }





            List<string> lstPresencedNames = new List<string>();
            if (cbPersence.SelectedItems != null && cbPersence.SelectedItems.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in cbPersence.SelectedItems)
                {
                    if (lstPresencedNames.Contains(pair.Key) == false)
                    {
                        lstPresencedNames.Add(pair.Key);
                    }
                }
            }


            if (
                dpFromDate != null || toDate != null
                || selectedProtocolNumber > 0
                || string.IsNullOrWhiteSpace(selectedProcotocolCommitte) == false
                || lstInvitedNames.Count > 0
                || lstPresencedNames.Count > 0
                )
            {
                protocols = context.Protocols.Where(p => (selectedProtocolNumber == 0 || p.pr_number == selectedProtocolNumber)
               &&
               (string.IsNullOrEmpty(selectedProcotocolCommitte) || p.c_name.Contains(selectedProcotocolCommitte))
               &&
               (fromDate == null || p.pr_date >= fromDate)
               &&
               (toDate == null || p.pr_date <= toDate)

               ).ToList();


                // intersecting with invitations :           
                List<Invitation> relevantInvitations = context.Invitations.Where(i => lstInvitedNames.Contains(i.pn_name)).ToList();

                if (relevantInvitations.Count > 0)
                {

                    protocols = (from p in protocols
                                 join i in relevantInvitations
                                 on p.pr_number equals i.pr_number
                                 where p.c_name == i.c_name
                                 select new Protocol
                                 {
                                     pr_number = p.pr_number,
                                     pr_title = p.pr_title,
                                     pr_date = p.pr_date
                                 }
                                 ).ToList().Distinct(new ProtocolsComparer()).ToList();
                }


                // intersecting with presence :           
                List<Presence> relevantPrecenses = context.Persence.Where(i => lstPresencedNames.Contains(i.pn_name)).ToList();

                if (relevantPrecenses.Count > 0)
                {

                    protocols = (from p in protocols
                                 join rp in relevantPrecenses
                                 on p.pr_number equals rp.pr_number
                                 where p.c_name == rp.c_name
                                 select new Protocol
                                 {
                                     pr_number = p.pr_number,
                                     pr_title = p.pr_title,
                                     pr_date = p.pr_date
                                 }
                                 ).ToList().Distinct(new ProtocolsComparer()).ToList();
                }


                if (protocols != null && protocols.Count > 0)
                {
                    lstResults.ItemsSource = protocols;
                }
                else
                {
                    List<Protocol> messages = new List<Protocol>();
                    Protocol message = new Protocol();
                    message.pr_title = "לא נמצאו תוצאות";
                    messages.Add(message);


                    lstResults.ItemsSource = messages;
                }
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
