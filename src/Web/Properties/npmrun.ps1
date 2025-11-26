$projectParent = (get-item $PSScriptRoot).Parent
$projectPath = $projectParent.FullName

Set-Location $projectPath
Clear-Host

$currentHash = (Get-FileHash '.\package.json').Hash
$oldHash = (Get-Content '.\node_modules\hash' -First 1 -ErrorAction SilentlyContinue | Out-String).Trim()

if ($currentHash -ne $oldHash) {
	npm install
	New-Item -ItemType Directory -Path '.\node_modules' -Force | Out-Null
	$currentHash | Out-File '.\node_modules\hash'
}

npm run dev