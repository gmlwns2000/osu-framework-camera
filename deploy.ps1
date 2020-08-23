$year = (Get-Date).Year
$monthDay = [int]([String](Get-Date).Month + [String](Get-Date).Day)
$revision = 0

try {
    $request = "https://api.nuget.org/v3/registration5-gz-semver2/osu.framework.camera/index.json"
    $entries = Invoke-WebRequest $request | ConvertFrom-Json | Select-Object -expand items
    $current = $entries[0].items.catalogEntry.version.Split(".")

    if ( ([int]($current[0]) -eq $year) -and ([int]($current[1]) -eq $monthDay) ) {
        $revision = [int]($current[2]) + 1
    }
}
catch {
    Write-Host "An error has occured. The package may not exist yet in NuGet."
}
finally {
    $version = [string]::Format("{0}.{1}.{2}", [string]$year, [string]$monthDay, [string]$revision)
    & dotnet.exe pack ./osu.Framework.Camera/osu.Framework.Camera.csproj -c Release -o output /p:Version=$version
    & dotnet.exe nuget push ./output/osu.Framework.Camera.$version.nupkg --api-key $env:NUGET_API_KEY --skip-duplicate --no-symbols true
}