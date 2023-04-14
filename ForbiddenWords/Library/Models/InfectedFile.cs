namespace Library.Models
{
	public class InfectedFile
	{
		public string Name { get; protected set; }
		public string NameWithoutExtension { get; protected set; }
		public string Path { get; protected set; }
		public long Size { get; protected set; }
		public List<ForbiddenWord> ForbiddenWords { get; protected set; }

		public InfectedFile(string name, string nameWithoutExtension, string path, long size, List<ForbiddenWord> forbiddenWords)
		{
			Name = name;
			NameWithoutExtension = nameWithoutExtension;
			Path = path;
			Size = size;
			ForbiddenWords = forbiddenWords;
		}
	}
}
