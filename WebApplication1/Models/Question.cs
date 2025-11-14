using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlateformeEvaluation.Models
{
    public class Question
    {
        public int Id { get; set; }
        
        [Required]
        public string Texte { get; set; } = string.Empty;
        
        public string Type { get; set; } = string.Empty;
        
        public int Points { get; set; }
        
        // Relations
        public virtual ICollection<ReponseCandidat> ReponsesCandidats { get; set; } = new List<ReponseCandidat>();
        public virtual ICollection<OptionReponse> OptionsReponse { get; set; } = new List<OptionReponse>();
    }
}