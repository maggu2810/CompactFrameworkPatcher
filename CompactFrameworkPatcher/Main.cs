/*******************************************************************************
 * Copyright (c) 2015 Markus Rathgeb.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 *
 * Contributors:
 *    Markus Rathgeb - initial API and implementation and/or initial documentation
 *******************************************************************************/

using System;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.Text;

namespace CompactFrameworkPatcher
{
	/* 
	 * Just stored to remember:
	 * 
     * mappableAssemblies.Add ("mscorlib", fwPkToken1);
     * mappableAssemblies.Add ("System", fwPkToken1);
     * mappableAssemblies.Add ("System.Data", fwPkToken1);
     * mappableAssemblies.Add ("System.Drawing", fwPkToken2);
     * mappableAssemblies.Add ("System.Web.Services", fwPkToken2);
     * mappableAssemblies.Add ("System.Windows.Forms", fwPkToken1);
     * mappableAssemblies.Add ("System.Xml", fwPkToken1);
     * mappableAssemblies.Add ("Microsoft.VisualBasic", fwPkToken2);
     *  
	 */

	/*
	 * This is currently patched:
	 * 
	 * patch ref: name=mscorlib version=2.0.0.0; public key token 'b77a5c561934e089' => '969db8053d3322ac'
	 * patch ref: name=System version=2.0.0.0; public key token 'b77a5c561934e089' => '969db8053d3322ac'
     * patch ref: name=System.Core version=3.5.0.0; public key token 'b77a5c561934e089' => '969db8053d3322ac'
     * 
     */

	class MainClass
	{
	    static readonly byte [] PUBLIC_KEY_TOKEN_CF = new byte [] { 0x96, 0x9d, 0xb8, 0x05, 0x3d, 0x33, 0x22, 0xac };
    	//static readonly byte [] PUBLIC_KEY_TOKEN_FW_1 = new byte [] { 0xb7, 0x7a, 0x5c, 0x56, 0x19, 0x34, 0xe0, 0x89 };
    	//static readonly byte [] PUBLIC_KEY_TOKEN_FW_2 = new byte [] { 0xb0, 0x3f, 0x5f, 0x7f, 0x11, 0xd5, 0x0a, 0x3a };
		
		private static string byteArrayToString (byte[] ba)
		{
			StringBuilder stringBuilder = new StringBuilder ();
			if (ba != null && ba.Length > 0) {
				for (int i = 0; i < ba.Length; i++) {
					stringBuilder.Append (ba [i].ToString ("x2"));
				}
			} else {
				stringBuilder.Append ("null");
			}
			return stringBuilder.ToString ();
		}
		
		private static void printAssemblyNameReference (AssemblyNameReference asmRef)
		{
			System.Console.WriteLine ("=== {0} ===", asmRef);
			System.Console.WriteLine ("  attributes {0}", asmRef.Attributes);
			System.Console.WriteLine ("  culture {0}", asmRef.Culture);
			System.Console.WriteLine ("  full name {0}", asmRef.FullName);
			System.Console.WriteLine ("  hash {0}", byteArrayToString (asmRef.Hash));
			System.Console.WriteLine ("  hash algorithm {0}", asmRef.HashAlgorithm);
			System.Console.WriteLine ("  has public key {0}", asmRef.HasPublicKey);
			System.Console.WriteLine ("  is retargetable {0}", asmRef.IsRetargetable);
			System.Console.WriteLine ("  is side by side compatible {0}", asmRef.IsSideBySideCompatible);
			System.Console.WriteLine ("  is windows runtime {0}", asmRef.IsWindowsRuntime);
			System.Console.WriteLine ("  meta data scope type {0}", asmRef.MetadataScopeType);
			System.Console.WriteLine ("  meta data token {0}", asmRef.MetadataToken);
			System.Console.WriteLine ("  name {0}", asmRef.Name);
			System.Console.WriteLine ("  public key {0}", byteArrayToString (asmRef.PublicKey));
			System.Console.WriteLine ("  public key token {0}", byteArrayToString (asmRef.PublicKeyToken));
			System.Console.WriteLine ("  version {0}", asmRef.Version);
		}

		private static void printAssemblyReferences (AssemblyDefinition asm)
		{
			Collection<AssemblyNameReference> asmRefs = asm.MainModule.AssemblyReferences;
			
			foreach (AssemblyNameReference asmRef in asmRefs) {
				printAssemblyNameReference (asmRef);
			}
		}

		private static void patchAssemblyNameReferencePublicKeyToken (AssemblyNameReference asmRef, byte[] tokenNew)
		{
			byte[] tokenCur = asmRef.PublicKeyToken;

			bool patch = tokenCur == null || tokenCur.Length != tokenNew.Length;
			if (!patch) {
				for (int i = 0; i < tokenCur.Length; ++i) {
					if (tokenCur [i] != tokenNew [i]) {
						patch = true;
						break;
					}
				}
			}
			
			if (!patch) {
				return;
			}

			System.Console.WriteLine (
				"patch ref: name={0} version={1}; public key token '{2}' => '{3}'",
				asmRef.Name,
				asmRef.Version,
				byteArrayToString(tokenCur),
				byteArrayToString(tokenNew)
			);

			asmRef.PublicKeyToken = tokenNew;
		}

		private static void patchAssemblyReferences (AssemblyDefinition asm)
		{
			Collection<AssemblyNameReference> asmRefs = asm.MainModule.AssemblyReferences;
			
			foreach (AssemblyNameReference asmRef in asmRefs) {
				if (asmRef.Name.Equals ("mscorlib") ||
					asmRef.Name.Equals ("System") ||
				    asmRef.Name.Equals ("System.Core")) {
					patchAssemblyNameReferencePublicKeyToken(asmRef, PUBLIC_KEY_TOKEN_CF);
				}
			}
		}

		public static void Main (string[] args)
		{
			int cnt = 0;

			string fileOld = args.Length > cnt ? args [cnt++] : null;
			string fileNew = args.Length > cnt ? args [cnt++] : null;
			
			if (fileOld == null) {
				return;
			}
			
			System.Console.WriteLine ("read: " + fileOld);
			AssemblyDefinition asm = AssemblyDefinition.ReadAssembly (fileOld);

			printAssemblyReferences (asm);

			if (fileNew == null) {
				return;
			}

			System.Console.WriteLine ("write: " + fileNew);
			patchAssemblyReferences (asm);
			asm.Write (fileNew);
		}
	}
}
