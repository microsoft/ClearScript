param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site")
{
    $projectPath = $project.Properties.Item("FullPath").Value
    
    $binPath = Join-Path $projectPath "bin"
    if (!(Test-Path $binPath))
    {
        New-Item -ItemType Directory -Force -Path $binPath
    }

    $filePath = Join-Path $installPath "v8\ClearScriptV8.ICU.dat"
    Copy-Item $filePath $binPath -Force
}
