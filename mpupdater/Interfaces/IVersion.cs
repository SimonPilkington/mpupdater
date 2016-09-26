namespace mpupdater
{
	public interface IVersion
	{
		bool Installed { get; }
		int CompareTo(IVersion other);
	}
}
