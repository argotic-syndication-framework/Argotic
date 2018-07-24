$dir = Split-Path $MyInvocation.MyCommand.Path
Write-host "My directory is $dir"

function CreateTOC ([string]$tocPath)
{
    $tocP = $tocPath+"\toc.yml"
    write-host "Creating Toc for:"$tocP
    if (-not (Test-Path $tocP)){
        $files = Get-ChildItem -Path $tocPath"/" -Recurse -Include *.md, *.json
            for ($i=0; $i -lt $files.Count; $i++) {
                $old_name = $files[$i].FullName
                $new_name = $files[$i].Name -replace '%2d', '-'
                Rename-Item $old_name $new_name
                $name = $files[$i].BaseName -replace '\W',' '
                $nameWithExtension = $new_name

                $toAdd = "- name: $name"
                $toAddd = "  href: $nameWithExtension "
                Add-Content $tocPath"/"toc.yml $toAdd
                Add-Content $tocPath"/"toc.yml $toAddd
                write-host "Added" $nameWithExtension "to" $tocP
            }
    }else
    {
        write-host $tocPath" already has a toc.yml"
    }

}
CreateTOC $dir"/Articles"
CreateTOC $dir"/RestAPIs"


