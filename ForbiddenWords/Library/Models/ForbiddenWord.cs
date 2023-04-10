namespace Library.Models
{
	public class ForbiddenWord
	{
		public string Word { get; protected set; }
		public int NumberOfUses { get; protected set; }
		public ForbiddenWord(string word, int numberOfUses)
		{
			Word = word;
			NumberOfUses = numberOfUses;
		}
	}
}
