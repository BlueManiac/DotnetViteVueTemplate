$projectParent = (get-item $PSScriptRoot).Parent
$projectName = $projectParent.Name
$projectPath = $projectParent.FullName

wt -w 0 split-pane --title $projectName -d $projectPath PowerShell -NoExit ./Properties/npmrun.ps1

wt -w 0 split-pane --title $projectName -d $projectPath

Set-Location $projectPath
Clear-Host

$env:DOTNET_WATCH_RESTART_ON_RUDE_EDIT = 'true'

dotnet watch