param(
    [string[]]$Assemblies,
    [string]$NamespaceFilter,
    [string]$CodecovToken)

$ErrorActionPreference = "stop"

Function Find-NuGet([string]$PackageName, [string]$VersionPattern)
{
	$items = (Get-ChildItem -Path "$($env:UserProfile)\.nuget\packages\$PackageName\" -Filter $VersionPattern);
    $lastItem = $items[$items.Length - 1]

	Write-Host "Using NuGet $($lastItem.FullName)"
	
	return $lastItem.FullName;
}

Function Join-List([string[]]$List, [string]$Format)
{
    $AppendedList = $List | ForEach-Object { [System.String]::Format($Format, $_) }
    return [System.String]::Join(" ", $AppendedList)
}

Function Publish-TestProject()
{
    Push-Location ".\source\LH.Forcas.Tests"
    & dotnet publish
    Pop-Location
}

Function Run-OpenCover([string[]]$Assemblies, [string]$NamespaceFilter)
{
    $NunitExePath = "$(Find-NuGet -PackageName 'NUnit.ConsoleRunner' -VersionPattern '3.6*')\tools\nunit3-console.exe"
	$OpenCoverExePath = "$(Find-NuGet -PackageName 'OpenCover' -VersionPattern '4.6*')\tools\OpenCover.Console.exe"

	Write-Host "Running Tests through OpenCover..."
	$TestsOutputFormat = "nunit3" #"AppVeyor"
    $AssembliesString = [System.String]::Join(" ", $Assemblies)
	
	& $($OpenCoverExePath) -register:user -target:"$NunitExePath" -targetargs:"--noheader $AssembliesString --result=TestResults.xml;format=$TestsOutputFormat" -returntargetcode -filter:"+[$NamespaceFilter]*" -excludebyattribute:"*.ExcludeFromCodeCoverage*;*.GeneratedCode*" -hideskipped:All -output:.\Coverage.xml
}

Function Upload-CoverageResultsToAppVeyor()
{
    $Uri = "https://ci.appveyor.com/api/testresults/nunit/$($env:APPVEYOR_JOB_ID)"
    Write-Host "Uploading Tests Results to AppVeyor ($Uri)..."
    
    $wc = New-Object 'System.Net.WebClient'
    $wc.UploadFile($Uri, (Resolve-Path .\TestResults.xml))
}

Function Install-Codecov()
{
    Write-Host "Installing Codecov..."
    & pip install codecov
}

Function Upload-CoverageResultsToCodecov([string]$CodecovToken)
{
    Write-Host "Uploading coverage report..."
    & codecov -f "Coverage.xml" -X gcov -t $CodecovToken
}

#Publish-TestProject

Run-OpenCover -Assemblies $Assemblies -NamespaceFilter $NamespaceFilter

Upload-CoverageResultsToAppVeyor

Install-Codecov

Upload-CoverageResultsToCodecov -CodecovToken $CodecovToken