$dir = Split-Path $MyInvocation.MyCommand.Path
Write-host "My directory is $dir"

function CleanUp ([string]$wikiPath, [string] $toPath )
{
    if (Test-Path $wikiPath"/"){
        if(-not (Test-Path $toPath"/"))
        {
            write-host "Creating new directory "$toPath
            New-Item -ItemType Directory -Path $toPath"/"
            write-host "Created new directory "$toPath
        }
        write-host "Coping all .md files from"$wikiPath"to"$toPath
        $path = $wikiPath+"/*.md"
        copy-Item -Path $path -Destination $toPath"/" -force
        write-host "Copied all .md files from"$wikiPath"to"$toPath
    }else
    {
        write-host $wikiPath" - Wiki Path does not exist"
    }

}
CleanUp $dir"/WikiClone" $dir"/Articles"