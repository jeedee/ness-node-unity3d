using UnityEngine;
using System;
using System.Collections;

namespace Ness
{
	public static class Op
	{
		public const char CREATE = 'c';
		public const char READ = 'r';
		public const char UPDATE = 'u';
		public const char DELETE = 'd';
		public const char EXECUTE = 'e';
	}
	
	public class NessExecutable : Attribute
	{
		//...
	}
}
