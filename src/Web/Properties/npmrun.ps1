$currentHash = (Get-FileHash '.\package.json').Hash
$oldHash = (Get-Content '.\node_modules\hash' -First 1 -ErrorAction SilentlyContinue | Out-String).Trim()

if ($currentHash -ne $oldHash) {
	pnpm install
	$currentHash | Out-File '.\node_modules\hash'
}

pnpm run dev