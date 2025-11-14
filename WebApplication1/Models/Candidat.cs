// namespace PlateformeEvaluation.Models
// {
//     public class Candidat
//     {
//         public int Id { get; set; }
//         public string Nom { get; set; }
//         public string Email { get; set; }
//         public string MotDePasse { get; set; }

//         public string Prenom { get; set; }
//         public ICollection<Tentative> Tentatives { get; set; }
//         public ICollection<Candidature> Candidatures { get; set; }


//     }
// }
namespace PlateformeEvaluation.Models
{
    public class Candidat
    {
        public int Id { get; set; }
        
        public string Nom { get; set; } = string.Empty;
        
        public string Prenom { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public string MotDePasse { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Tentative> Tentatives { get; set; } = new List<Tentative>();
        
        public ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    }
}