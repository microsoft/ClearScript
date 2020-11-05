param($installPath, $toolsPath, $package, $project)

if ($project.Type -eq "Web Site")
{
    $projectPath = $project.Properties.Item("FullPath").Value

    $filePath = Join-Path $projectPath "bin\ClearScriptV8.win-x64.dll"
    if (Test-Path $filePath)
    {
        Remove-Item $filePath -Force
    }
}
