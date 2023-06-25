# Check OS
if ($env:OS -eq "Windows_NT") {
    Write-Host "OS is Windows"
    # Check Architecture
    if ($env:PROCESSOR_ARCHITECTURE -eq "AMD64") {
        $RID = "win-x64"
    } elseif ($env:PROCESSOR_ARCHITECTURE -eq "x86") {
        $RID = "win-x86"
    } elseif ($env:PROCESSOR_ARCHITECTURE -eq "ARM64") {
        $RID = "win-arm64"
    } elseif ($env:PROCESSOR_ARCHITECTURE -eq "ARM") {
        $RID = "win-arm"
    }
} else {
    $uname_s = bash -c 'uname -s'
    $uname_m = bash -c 'uname -m'
    
    Write-Host "OS is $uname_s"
    if ($uname_s -eq "Linux") {
        # Check Architecture
        if ($uname_m -eq "x86_64") {
            $RID = "linux-x64"
        } elseif ($uname_m -eq "armv7l") {
            $RID = "linux-arm"
        } elseif ($uname_m -eq "aarch64") {
            $RID = "linux-arm64"
        }
    } elseif ($uname_s -eq "Darwin") {
        # Check Architecture
        if ($uname_m -eq "x86_64") {
            $RID = "osx-x64"
        } elseif ($uname_m -eq "arm64") {
            $RID = "osx-arm64"
        }
    }
}

if ($null -eq $RID) {
    Write-Host "Could not determine Runtime Identifier (RID)"
    exit 1
}

Write-Host "Runtime Identifier (RID): $RID"

# Check if folder already has files, and offer to clean if so
if (Test-Path ./publish/$RID) {
    Write-Host "Folder ./publish/$RID already exists"
    $clean = Read-Host -Prompt "Clean folder? [Y/n] "

    if ($clean -ne "" -and $clean.ToUpper() -ne "Y") {
        Write-Host "Aborting build process"
        exit
    }

    Write-Host "Cleaning folder ./publish/$RID"
    Remove-Item -Recurse -Force ./publish/$RID
} else {
    # If no cleanup was necessary, at least prompt to continue to ensure the process doesn't go without any user confirmation
    $continue = Read-Host -Prompt "Continue? [Y/n] "
    if ($continue -ne "" -and $continue.ToUpper() -ne "Y") {
        Write-Host "Aborting build process"
        exit
    }
}

# Continue your build process here...
# PublishReadyToRun: Precompile assemblies for faster startup, straight to machine code
#                    https://docs.microsoft.com/en-us/dotnet/core/deploying/ready-to-run
#                    https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot
# PublishTrimmed: Trim unused assemblies
#                 https://docs.microsoft.com/en-us/dotnet/core/deploying/trim-self-contained
# PublishToSingleFile: Publish as a single file
#                      https://docs.microsoft.com/en-us/dotnet/core/deploying/single-file
& dotnet publish -c Release -r $RID -p:PublishReadyToRun=true -p:PublishTrimmed=false -p:PublishSingleFile=true -o ./publish/$RID
# -p:PublishTrimmed does not work in WPF projects

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded"
}
else {
    Write-Host "Build failed"
}