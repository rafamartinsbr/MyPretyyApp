using System;
using System.IO;
using System.Linq;
using System.Configuration;
using System.Globalization;
using System.Buffers.Binary;
using System.Collections;
using System.Buffers.Text;
using System.Threading;
using Dapper;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using Microsoft.VisualBasic;
using System.ComponentModel.Composition;
using ExternalSystemApi;

namespace MyPretyyApp
{
	class Program
	{
		//a temp char array
		private static char[] temp;
		private static readonly char[] temp2 = "allowed".ToCharArray(); //todo: should move this to a config file ?
		private static int _last = 0; //this is the last vlaue used by the logic
		private static IImportantSystemApi api;

		static void Main(string[] args) //obs: performance should be better now after the refactoring done (now we're using async reads)
		{
			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: starting app...");

			TextReader a = new StreamReader(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(new char[] { '/' }) + "/" + @"data.txt"); //making sure it ends with a '\'

			//reads the file's content asynchronously into a buffer var
			var b = a.ReadAsync(temp = temp == null ? new char[8] : temp, 0, temp.Length).Result;

			//this is not allowed - we must check it before publishing the data!
			if (b < 8) throw new InvalidOperationException("todo: descriptive business rule message");

			//logging info
			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: data is at least {b} long. Continuinig the process");

			GetNewBuffer(ref temp); //here we continue the process

			var element = temp.First(i => i == '\0'); //this is the string represatation of an empty char
			int posIndex = temp.ToList().IndexOf(element); //and we need the index of it to use latre, and, therefore retrieving it thus

			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: vi leser mer data nå...");
			var data = a.ReadAsync(temp = temp == null ? new char[8] : temp, posIndex, temp.Length - posIndex).Result; //reads the file's content adding on top of data already there
			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: read {data} lines");

			var processedData = SelectTheData(); //processing the new chunck of data since we know it is valid now (ie can be publsihed)

			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: all data is successfully processed and completely ready for export operation");

			//preparing the data
			processedData.RemoveAll(filter => filter.Item2 == ConfigurationManager.AppSettings["separator"]);
			processedData.RemoveAll(filter => string.IsNullOrWhiteSpace(filter.Item2));

			//todo: adapt and send data to their api
			//ConvertToExternalDataFormat(processedData)
			//api.PublishData();

			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: all data is published !");

			Console.ReadLine();
		}

		private static List<Tuple<int, string>> SelectTheData()
		{
			try
			{
				Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: selecting the data");

				//processing it in memory here to deliver a low-latency solution
				var p = temp.Where(c => !temp2.Contains(c)).ToArray().Select(e => new Tuple<int, string>(GetNewValueOrCalculateIt(e), e.ToString())).ToList();

				Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: a total of {p.Count} entries returned");

				return p;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log exception");
				throw ex; //rethrowing
			}
		}

		private static void GetNewBuffer(ref char[] temp)
		{
			var @new = new List<char>(temp.Length + 64); //max allowed data size is 64 (limit is set by their api)
			
			@new.AddRange(temp);
			temp = new char[@new.Capacity];
			
			int i = 0;
			foreach (var item in @new)
			{
				temp[i++] = item;
			}
		}

		private static int GetNewValueOrCalculateIt(char e)
		{
			var separator = ConfigurationManager.AppSettings["separator"];

			Console.WriteLine($"[{DateTime.Now.ToString("HH:MM:ss")}] Log info: {e}");
			return e.ToString() == separator ? _last+=1 : _last;
		}
	}
}