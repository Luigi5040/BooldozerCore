using System;
using System.IO;
using Booldozer.Models.Bin;

namespace Booldozer
{
	class MainClass
	{
		public static void Main(string[] args)
		{
            BinModel bin = new BinModel(args[0]);
        }
	}
}
