using MySql.Data.Entity;
using System.Data.Entity;

namespace knesset_app.DBEntities
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class KnessetContext : DbContext
    {
        public KnessetContext() : base("knesset")
        {
            Database.SetInitializer<KnessetContext>(null); // prevent migration process each time the context is initialized
        }

        public DbSet<WordsGroup> WordsGroups { get; set; }
        public DbSet<WordInGroup> WordInGroups { get; set; }
        public DbSet<Word> Words { get; set; }
        public DbSet<Presence> Persences { get; set; }
        public DbSet<Phrase> Phrases { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<ParagraphWord> ParagraphWords { get; set; }
        public DbSet<Paragraph> Paragraphs { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Committee> Committees { get; set; }
        public DbSet<Protocol> Protocols { get; set; }
    }
}
