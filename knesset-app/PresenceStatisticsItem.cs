namespace knesset_app
{
    class PresenceStatisticsItem
    {
        // represents one row in the presence statistics grid view
        public string pn_name { get; set; }
        public float Presence { get; set; }
        public int TimesPresent { get; set; }
    }
}
