using System.ComponentModel.DataAnnotations;

namespace WordleHelper.Core
{
    public class WordleHelperInput
    {
        [Required]
        [StringLength(maximumLength: 5, MinimumLength = 5, ErrorMessage = "Skeleton Must Be 5 Characters")]
        public string Skeleton { get; set; } = "-----";
        [Required]
        [StringLength(maximumLength: 5, MinimumLength = 5, ErrorMessage = "Skeleton Must Be 5 Characters")]
        public string WrongPositionSkeleton { get; set; } = "-----";
        public string KnownLetters { get; set; } = "";
        public string BlockedLetters { get; set; } = "";
    }

    public class WordleSolution
    {
        public string SolutionText { get; set; } = "";
        public IEnumerable<string> Solutions { get; set; } = new List<string>();
        public int Count { get; set; } = 0;
    }
}
