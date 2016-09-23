using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpupdater
{
	public interface IVersion
	{
		bool Installed { get; }
		int CompareTo(IVersion other);
	}
}
