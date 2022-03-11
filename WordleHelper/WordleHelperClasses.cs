using System.ComponentModel.DataAnnotations;

namespace WordleHelper
{
    public class DictionaryData
    {
        public List<string> Words { get; set; } = new List<string>();
    }
    public class WordleHelperInput
    {
        [Required]
        [StringLength(maximumLength: 5, MinimumLength = 5, ErrorMessage = "Skeleton Must Be 5 Characters")]
        public string Skeleton { get; set; } = "";
        [Required]
        [StringLength(maximumLength: 5, MinimumLength = 5, ErrorMessage = "Skeleton Must Be 5 Characters")]
        public string WrongPositionSkeleton { get; set; } = "";
        public string KnownLetters { get; set; } = "";
        public string BlockedLetters { get; set; } = "";
    }
    public class WordleSolution
    {
        public string Solution { get; set; } = "";
        public int Count { get; set; } = 0;
    }
}
