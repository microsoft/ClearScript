
-----------------------------
ClearScript 5.x NuGet Package
-----------------------------

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
