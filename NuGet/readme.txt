
-----------------------------
ClearScript 5.x NuGet Package
-----------------------------

IMPORTANT: This package requires the Visual C++ Redistributable libraries.
Download and install both the 32-bit and 64-bit versions:

    https://aka.ms/vs/16/release/vc_redist.x86.exe
    https://aka.ms/vs/16/release/vc_redist.x64.exe

IMPORTANT: If you're adding this package to an ASP.NET project, you must
exclude ClearScript's mixed-mode assemblies from ASP.NET compilation. To
do so, merge the following into all your Web.config files:

    <system.web>
      <compilation>
        <assemblies>
          <remove assembly="ClearScriptV8-64" />
          <remove assembly="ClearScriptV8-32" />
        </assemblies>
      </compilation>
    </system.web>

Thanks for using ClearScript!
