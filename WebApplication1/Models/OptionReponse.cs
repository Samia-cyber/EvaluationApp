using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlateformeEvaluation.Models
{
    public class OptionReponse
    {
        public int Id { get; set; }
        
        [Required]
        public string Texte { get; set; } = string.Empty;
        
        public bool EstCorrecte { get; set; }
        
        // Relation avec Question
        [ForeignKey("Question")]
        public int QuestionId { get; set; }
        public virtual Question? Question { get; set; }
        
        // Relation avec ReponseCandidat
        public virtual ICollection<ReponseCandidat> ReponsesCandidats { get; set; } = new List<ReponseCandidat>();
    }
}