param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site")
{
    $projectPath = $project.Properties.Item("FullPath").Value
    
    $binPath = Join-Path $projectPath "bin"
    if (!(Test-Path $binPath))
    {
        New-Item -ItemType Directory -Force -Path $binPath
    }

    $filePath = Join-Path $installPath "runtimes\win-x86\native\ClearScriptV8.win-x86.dll"
    Copy-Item $filePath $binPath -Force
}
