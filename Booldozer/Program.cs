using System;
using System.IO;
using Booldozer.Models.Bin;
using Booldozer.Models.Mdl;

namespace Booldozer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            //BinModel bin = new BinModel(args[0]);
			MdlModel mdl = new MdlModel(args[0]);
        }
	}
}
