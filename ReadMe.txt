---------------
I. Introduction
---------------

Welcome to ClearScript!

ClearScript is a library that allows you to add scripting to your .NET
applications. It supports JScript and VBScript out of the box and in theory can
work with other Windows Script engines.

ClearScript 5 adds support for the V8 high-performance open-source JavaScript
engine. It allows you to use V8 with the same managed API and host integration
features as JScript and VBScript. V8 provides better performance, however, and
is more suitable for multi-threaded applications and asynchronous server-side
scripting.

See ClearScript\doc for information about using ClearScript.

------------------------
II. Building ClearScript
------------------------

The provided project and solution files support Visual Studio 2012 and 2013.
They produce architecture-neutral managed libraries that target .NET Framework
4.0. ClearScript has been tested with .NET Framework 4.5 as well. It does not
support older environments. The output directory is bin\[Debug|Release].

There are two ways to build ClearScript - with and without V8 support.

If you don't need V8 support, simply build the ClearScript.NoV8 solution using
Visual Studio. Note that this solution does not include test projects.

In order to build full ClearScript with V8 support, you must first acquire,
build, and import V8:

1. NOTE: This procedure and the V8Update script are provided for your
   convenience. ClearScript does not include V8 source code, nor does it come
   with any third-party software required to download and build V8. Rights to
   V8 and its prerequisites are provided by their rights holders.

2. Install Subversion (http://subversion.apache.org/packages.html) and add it
   to your executable path.

3. Open a Visual Studio developer command prompt and run the V8Update script
   from your ClearScript root directory:

      C:\ClearScript> V8Update [/N] [Debug|Release] [Latest|Tested|Revision#]

   This script downloads the V8 source code and its prerequisites, builds
   32-bit and 64-bit V8 shared libraries, and imports the results into
   ClearScript. It requires approximately 2GB of additional disk space and does
   not perform any permanent software installation on your machine.

   The optional "/N" flag causes V8Update to reuse previously downloaded files
   if possible. It's useful for switching between Debug and Release versions of
   V8 and for testing local V8 modifications.

   Specifying "Debug" or "Release" is optional; the default is Release. The
   selected V8 variant will then be used for all ClearScript configurations.

   By default, V8Update downloads a V8 revision that has been tested with the
   current version of ClearScript. If you'd like to use a specific revision
   instead, place the desired trunk revision number on the V8Update command
   line. To use the latest stable trunk revision, specify "Latest". Go to
   http://code.google.com/p/v8/source/list to view V8's revision history.

You are now ready to build the full ClearScript solution using Visual Studio.

NOTE: The first time you open the solution, Visual Studio may prompt you to
upgrade one or more projects to the latest platform toolset or .NET Framework.
We recommend that you select "Cancel" or "Don't Upgrade".

OPTIONAL: The ClearScript distribution includes a copy of the ClearScript
Library Reference in Compiled HTML (.CHM) format. If you'd like to rebuild this
file, use Sandcastle Help File Builder (SHFB, http://shfb.codeplex.com) with
the provided SHFB project file (ClearScript\doc\Reference.shfbproj).

------------------------------------------------------------
III. Building strong-named ClearScript assemblies (optional)
------------------------------------------------------------

ClearScript now includes optional support for building strong-named assemblies.
Use the following one-time procedure to enable this feature:

1. If the ClearScript solution is open in Visual Studio, close it.

2. Generate a cryptographic key pair in your ClearScript root directory:

      C:\ClearScript> sn -k ClearScript.snk
      
3. Open the ClearScript solution in Visual Studio.

4. Click "Build" -> "Transform All T4 Templates".

5. Rebuild the solution.

Once you've performed these steps, the ClearScript assemblies you build will
have strong names.
      
---------------------------------------------------------------
IV. Integrating and deploying ClearScript with your application
---------------------------------------------------------------

Once you've built ClearScript, here's how to add it to your application:

1. Right-click your project in Visual Studio and select "Add Reference...".

2. In the Reference Manager window, click "Browse..." and locate your
   ClearScript output directory (see above). Select ClearScript.dll, click
   "Add", and then click "OK" to exit Reference Manager.

3. IMPORTANT: If you're using V8, you must also copy the following files from
   your ClearScript output directory to your application's directory:

      ClearScriptV8-32.dll
      ClearScriptV8-64.dll
      v8-ia32.dll
      v8-x64.dll
      
   For ASP.NET projects, we recommend that you add these assemblies as content
   files at the root of your web application and set their "Copy to Output
   Directory" properties to "Do not copy". 

4. IMPORTANT: If Visual Studio is not installed on your deployment machine,
   you must install 32-bit and 64-bit Visual C++ Redistributable packages:

      Visual Studio 2012:
      http://www.microsoft.com/en-us/download/details.aspx?id=30679
      
      Visual Studio 2013:
      http://www.microsoft.com/en-us/download/details.aspx?id=40784

------------------------------------
V. Debugging with ClearScript and V8
------------------------------------

V8 does not support standard Windows script debugging. Instead, it implements
its own TCP/IP-based debugging protocol. A convenient way to debug JavaScript
code running in V8 is to use the open-source Eclipse IDE:

1. Install Eclipse:

      http://www.eclipse.org/downloads/

2. Install Google Chrome Developer Tools for Java:

    a. Launch Eclipse and click "Help" -> "Install New Software...".
    b. Paste the following URL into the "Work with:" field:

          http://chromedevtools.googlecode.com/svn/update/dev/

    c. Select "Google Chrome Developer Tools" and complete the dialog.
    d. Restart Eclipse.

3. Enable script debugging in your application by invoking the V8ScriptEngine
   constructor with V8ScriptEngineFlags.EnableDebugging and an available TCP/IP
   port number. The default port number is 9222.

4. Attach the Eclipse debugger to your application:

    a. In Eclipse, select "Run" -> "Debug Configurations...".
    b. Right-click on "Standalone V8 VM" and select "New".
    c. Fill in the correct port number and click "Debug".

Note that you can also attach Visual Studio to your application for
simultaneous debugging of script, managed, and native code.

-------------------
VI. V8 Known Issues
-------------------

1. V8 doesn't support indexers - properties with one or more parameters. Given
   the general syntax "A.B(C,D) = E" where A is an external object, JScript
   performs a single operation that assigns E to A's property B with index
   arguments C and D. This syntax allows for multiple indices and arbitrary
   index types. V8 interprets it as an attempt to use a value on the left side
   of an assignment - something that makes no sense in JavaScript. JScript's
   behavior appears to be an extension, but it's a convenient one because
   indexers are common in the CLR.

   WORKAROUND: ClearScript supports the alternate syntax "A.B.set(C,D,E)".

2. V8 doesn't support default properties. This is only an issue in conjunction
   with (1). The problematic syntax is of the form "A(B, C) = D", which in
   JScript means "assign D to external object A's default property with index
   arguments B and C".

   WORKAROUND: ClearScript supports the alternate syntax "A.set(B,C,D)".

3. V8 treats properties and methods identically. A method call is simply the
   invocation of a property. This causes ambiguity when an object has both a
   property and a method with the same name. An example of this in the CLR is
   an instance of System.Collections.Generic.List with LINQ extensions; such
   an object has both a property and a method named Count.

   WORKAROUND: None.
