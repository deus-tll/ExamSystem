using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Models
{
	public class ProgressChanged
	{
		public long CurrentCountElements { get; set; }
		public string? FullName { get; set; }
		public double Progress { get; set;}
	}
}
