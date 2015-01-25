﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using ZeroMQ;

namespace ZeroMQ.Test
{
	static partial class Program
	{
		public static void Interrupt(IDictionary<string, string> dict, string[] args)
		{
			//
			// Interrupt
			//
			// Authors: Pieter Hintjens, Uli Riehm
			//

			if (args == null || args.Length == 0)
			{
				args = new string[] { "World" };
			}

			string name = args[0];

			using (var context = ZContext.Create())
			using (var responder = ZSocket.Create(context, ZSocketType.REP))
			{
				Console.CancelKeyPress += (sender, e) =>
				{
					e.Cancel = true;

					ZError _error;
					if (!context.Shutdown(out _error))
					{
						Console.WriteLine(_error);
					} /**/
				};

				responder.Bind("tcp://*:5555");

				var error = ZError.None;
				ZFrame request;
				while (true)
				{
					if (Console.KeyAvailable)
					{
						ConsoleKeyInfo info = Console.ReadKey(true);
						/* if (info.Modifiers == ConsoleModifiers.Control && info.Key == ConsoleKey.C)
						{
							context.Shutdown();
						} /**/
						if (info.Key == ConsoleKey.Escape)
						{
							context.Shutdown();
						}
					} /**/

					if (null == (request = responder.ReceiveFrame(ZSocketFlags.DontWait, out error)))
					{
						if (error == ZError.EAGAIN)
						{
							error = ZError.None;
							Thread.Sleep(64);

							continue;
						} /**/
						if (error == ZError.ETERM)
							break;	// Interrupted
						throw new ZException(error);
					}

					using (request)
					{
						Console.Write("Received: {0}!", request.ReadString());

						Console.WriteLine(" Sending {0}... ", name);
						using (var response = new ZFrame(name))
						{
							if (!responder.Send(response, out error))
							{
								if (error == ZError.ETERM)
									break;	// Interrupted
								throw new ZException(error);
							}
						}
					}
				}

				if (error == ZError.ETERM)
				{
					Console.WriteLine("Terminated! You have pressed CTRL+C or ESC.");
					return;
				}
				throw new ZException(error);
			}
		}
	}
}