// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"

open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO
open Fake.Testing.NUnit3

#if MONO
#else
#load "packages/build/SourceLink.Fake/tools/Fake.fsx"
open SourceLink
#endif


// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Eclosing.Core"

// Short summary of the project
// (used as description in AssemblyInfo and as a short summary for NuGet package)
let summary = "plateforme standard de developpement eclosing"

// Longer description of the project
// (used as a description for NuGet package; line breaks are automatically cleaned up)
let description = "Project has no description; update build.fsx"

// List of author names (for NuGet package)
let authors = [ "lmc" ]

// Tags for your project (for NuGet package)
let tags = "eclosing"

// File system information
let solutionFile  = sprintf "%s.sln" project    



// Git configuration (used for publishing documentation in gh-pages branch)
// The profile where the project is posted
let gitOwner = "Update GitHome in build.fsx"
let gitHome = sprintf "%s/%s" "https://github.com" gitOwner

// The name of the project on GitHub
let gitName = "eclosing"

// The url for the raw files hosted
let gitRaw = environVarOrDefault "gitRaw" "https://raw.githubusercontent.com/Update GitHome in build.fsx"

// --------------------------------------------------------------------------------------
// END TODO: The rest of the file includes standard build steps
// --------------------------------------------------------------------------------------

// Read additional information from the release notes document
let release = LoadReleaseNotes "RELEASE_NOTES.md"


let config =
    getBuildParamOrDefault "BuildConfig" "build.config"
    |> readConfig


let getProperty name  defaultValue =
    let confDeploymentEnvironment = config.DocumentElement.GetElementsByTagName(name)
    let configValue = if (confDeploymentEnvironment.Count>0) then confDeploymentEnvironment.Item(0).InnerText else String.Empty
    let defValue = if String.IsNullOrEmpty(configValue) then defaultValue else configValue

    getBuildParamOrDefault name defValue

let configuration = "Release"
let coreFramework = "netcoreapp1.1"
let deploymentEnvironment = getProperty "DeploymentEnvironment" "debug"
let target = getProperty "Target" @"RunTests"
let solutionDir = getProperty "SolutionDir" Environment.CurrentDirectory
let resetDB = bool.Parse(getProperty "ResetDB" "true")
let runTests =  bool.Parse(getProperty "RunTests" "true")
let verbosity = getProperty "Verbosity" "detailed"

let nameDatabase = sprintf "%s_%s_%s"(getProperty "NameDatabase" project) Environment.MachineName deploymentEnvironment

let dataPathBaseDeDonnees = getProperty "DataPathBaseDeDonnees" (sprintf "%s/data/%s" solutionDir project)

if (dataPathBaseDeDonnees = sprintf "%s/data/%s" solutionDir project) then
    Path.Combine( solutionDir,"data")
    |> Directory.CreateDirectory
    |> ignore

let logPathBaseDeDonnees = getProperty "LogPathBaseDeDonnees" (sprintf "%s/data/%s" solutionDir project)
let serverDB = getProperty "ServerDB" "localhost"
let loginDB = getProperty "LoginDB" "sa"
let passwordDB = getProperty "PasswordDB" "yourStrong(!)Password"

let buildDir  = Path.Combine(solutionDir,"bin", deploymentEnvironment)
let buildServiceDir  = Path.Combine(buildDir,sprintf "%s.Service" project   ) 
let buildBatchDir  =Path.Combine(buildDir,sprintf "%s.Service.Batch" project) 
let buildDBDir  = Path.Combine(buildDir,sprintf "%s.DB" project) 
let buildwwwRootDir  = Path.Combine(buildDir,"wwwroot")

let staticSiteToDeployDir = @"src/wwwroot/"
let dbToDeployDir = sprintf @"src/%s.DB/" project
let serviceToDeployDir = sprintf @"src/%s.Service/" project
let batchToDeployDir = sprintf @"src/%s.Service.Batch/" project
let serviceTestDir = sprintf @"tests/%s.Service.Tests/" project



let fileConfig =
    if deploymentEnvironment = "debug" then
        "build.config"
    else
        sprintf "build.%s.config"  deploymentEnvironment


log <| sprintf "deploymentEnvironment : %s" deploymentEnvironment
log <| sprintf "target : %s" target
log <| sprintf "verbosity : %s" verbosity
log <| sprintf "solutionDir : %s" solutionDir
log <| sprintf "nameDatabase : %s" nameDatabase
log <| sprintf "dataPathBaseDeDonnees : %s" dataPathBaseDeDonnees
log <| sprintf "logPathBaseDeDonnees : %s" logPathBaseDeDonnees
log <| sprintf "serverDB : %s" serverDB
log <| sprintf "loginDB : %s" loginDB
log <| sprintf "passwordDB : %s" passwordDB

sprintf "file config used  : %s" fileConfig
|> log 



// Helper active pattern for project types
let (|Fsproj|Csproj|Vbproj|Shproj|) (projFileName:string) =
    match projFileName with
    | f when f.EndsWith("fsproj") -> Fsproj
    | f when f.EndsWith("csproj") -> Csproj
    | f when f.EndsWith("vbproj") -> Vbproj
    | f when f.EndsWith("shproj") -> Shproj
    | _                           -> failwith (sprintf "Project file %s not supported. Unknown project type." projFileName)


type configTransformation = 
    {
        filePath:string;
        transformTarget:string;
        outputFilePath: string;
    }

type envConfFile =
    {
        filePath:string;
        outputFilePath: string;
    }

let dotnet wd action timespan=
    let startProcess (x: Diagnostics.ProcessStartInfo)  = 
        x.WorkingDirectory <- wd
        x.FileName <- "dotnet"
        x.Arguments <- action

    let result = ExecProcessWithLambdas startProcess timespan false  log log 
    if (result <>0) then raise (Exception(sprintf "could not process action %s" action))

let transformConfigFiles target baseOutput=
    !! (sprintf "src/**/appsettings.%s.json" deploymentEnvironment)
    |> Seq.map (fun path ->
        let fileName = Path.GetFileName(path)
        ensureDirExists <| DirectoryInfo(baseOutput)
        //create the folder for the specified project in the baseOutput
        let srcfolder = Path.Combine(solutionDir,"src")
        let currentProjectFolder = path.Substring(srcfolder.Length,path.Length-srcfolder.Length-fileName.Length)

        let outputPath = 
            currentProjectFolder.Split('\\')
            |> Seq.fold (fun path newFolder ->
                ensureDirectory path
                if not <| String.IsNullOrEmpty(newFolder) then 
                    let newPath = Path.Combine(path, newFolder)
                    ensureDirectory newPath
                    newPath
                else   
                    path
            ) baseOutput

        ensureDirExists <| DirectoryInfo( outputPath)
        {  
            filePath = path
            outputFilePath = outputPath @@ "appsettings.json"
        } 
    
    )
    |> Seq.iter (fun conf->
        log <| sprintf "copying from %s to %s " conf.filePath conf.outputFilePath 
        CopyFile conf.outputFilePath conf.filePath
    )   


// Copies binaries from default VS location to expected bin folder
// But keeps a subdirectory structure for each project in the
// src folder to support multiple project outputs
Target "CopyBinaries" (fun _ ->

    !! "src/**.Service*/project.json"
    |>  Seq.map (fun f -> ( (System.IO.Path.GetDirectoryName f) </> "bin" </> "Release" </> coreFramework , solutionDir </> "bin" </> deploymentEnvironment  </>(System.IO.Path.GetFileName <| System.IO.Path.GetDirectoryName f)))
    |>  Seq.iter(fun (fromDir, toDir) -> 
        log <| sprintf "from dir %s to dir %s" fromDir toDir 
        CopyDir toDir fromDir (fun x -> 
                [
                    "appSettings.json"
                ]
                |> Seq.exists (fun c -> x.Contains(c))
                |> not
        )
    )
    
    if deploymentEnvironment = "debug" then 
        !! "src/**/appSettings.json"
        -- "src/**/bin/**/*.json"
        |>  Seq.map (fun f -> 
            let fi = FileInfo <| f
            (f,  "bin" </> deploymentEnvironment </> fi.Directory.Name </> fi.Name )
        )
        |> Seq.iter (fun (fromFile,toFile) ->
            log <| sprintf "from %s to %s" fromFile toFile 
            CopyFile toFile fromFile 
        )
    else transformConfigFiles deploymentEnvironment buildDir

)

//DB Stuff
let transformSqlScripts buildDBPath nameDB dataPathDB logPathDB =
    !! (sprintf "bin/%s/%s.DB/**/*.sql" deploymentEnvironment project)
    |> Seq.iter (ReplaceInFile (fun x -> x.Replace("{{dbName}}",nameDB).Replace("{{dataPath}}",dataPathDB).Replace("{{logPath}}",logPathDB)))


let buildDBTest buildDBPath  loginDB passwordDB serverDB =
    log <| sprintf " BuildingDB " 
    
    !! (sprintf "bin/%s/%s.DB/**/*.sql" deploymentEnvironment project )
    -- (sprintf "bin/%s/%s.DB/**/*Import*.sql" deploymentEnvironment project )
    |> Seq.iter ( fun f ->

        log <| sprintf " running sql script : %s" f
        let cmdParams = sprintf "-b -U %s -P %s -S %s -i \"%s\" " loginDB passwordDB serverDB f

        let startProcess (x: Diagnostics.ProcessStartInfo)  = 
            x.FileName <- "sqlcmd"
            x.Arguments <-cmdParams

        let result = ExecProcessWithLambdas startProcess (TimeSpan.FromSeconds(float <| 30)) false  log log 
        if (result <>0) then raise (Exception(sprintf "could not process file %s" f))
    )

    log <| sprintf " BuildingDB finished "




// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs [
        sprintf "bin/%s" deploymentEnvironment; 
        "temp"
    ]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

let build() =
    !! "src/**/project.json"
    ++ "tests/**/project.json"
    |> Seq.iter(fun proj ->
        let dotnet' = dotnet <| System.IO.Path.GetDirectoryName proj
        dotnet' "restore" <| TimeSpan.FromSeconds(float <| 120)
        dotnet' "build --configuration Release" <| TimeSpan.FromSeconds(float <| 60)
    )

    let fromDir = solutionDir </> "src" </> "Eclosing.Core.DB"
    let toDir = solutionDir </> "bin" </> deploymentEnvironment </> "Eclosing.Core.DB"
    log <| sprintf "from dir %s to dir %s" fromDir toDir 
    CopyDir toDir fromDir (fun x -> true)


Target "Build" (fun _ ->
    build()
    
)


let runTestSuite()= 
    !! "tests/**/project.json"
    |> Seq.iter(fun proj ->
            let dotnet' = dotnet <| System.IO.Path.GetDirectoryName proj
            dotnet' "test" <| TimeSpan.FromSeconds(float <| 120)
      )

 

Target "RunTests" <| fun _ ->
    runTestSuite()


// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "CreateDB" (fun _ ->
    transformSqlScripts buildDBDir nameDatabase dataPathBaseDeDonnees logPathBaseDeDonnees
    buildDBTest buildDBDir loginDB passwordDB serverDB
)



// --------------------------------------------------------------------------------------
// Generate the documentation


let fakePath = "packages" </> "build" </> "FAKE" </> "tools" </> "FAKE.exe"
let fakeStartInfo script workingDirectory args fsiargs environmentVars =
    (fun (info: System.Diagnostics.ProcessStartInfo) ->
        info.FileName <- System.IO.Path.GetFullPath fakePath
        info.Arguments <- sprintf "%s --fsiargs -d:FAKE %s \"%s\"" args fsiargs script
        info.WorkingDirectory <- workingDirectory
        let setVar k v =
            info.EnvironmentVariables.[k] <- v
        for (k, v) in environmentVars do
            setVar k v
        setVar "MSBuild" msBuildExe
        setVar "GIT" Git.CommandHelper.gitPath
        setVar "FSI" fsiPath)

/// Run the given buildscript with FAKE.exe
let executeFAKEWithOutput workingDirectory script fsiargs envArgs =
    let exitCode =
        ExecProcessWithLambdas
            (fakeStartInfo script workingDirectory "" fsiargs envArgs)
            TimeSpan.MaxValue false ignore ignore
    System.Threading.Thread.Sleep 1000
    exitCode

// Documentation
let buildDocumentationTarget fsiargs target =
    trace (sprintf "Building documentation (%s), this could take some time, please wait..." target)
    let exit = executeFAKEWithOutput "docs/tools" "generate.fsx" fsiargs ["target", target]
    if exit <> 0 then
        failwith "generating reference documentation failed"
    ()

Target "GenerateReferenceDocs" (fun _ ->
    buildDocumentationTarget "-d:RELEASE -d:REFERENCE" "Default"
)

let generateHelp' fail debug =
    let args =
        if debug then "--define:HELP"
        else "--define:RELEASE --define:HELP"
    try
        buildDocumentationTarget args "Default"
        traceImportant "Help generated"
    with
    | e when not fail ->
        traceImportant "generating help documentation failed"

let generateHelp fail =
    generateHelp' fail false

Target "GenerateHelp" (fun _ ->
    DeleteFile "docs/content/release-notes.md"
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    DeleteFile "docs/content/license.md"
    CopyFile "docs/content/" "LICENSE.txt"
    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp true
)

Target "GenerateHelpDebug" (fun _ ->
    DeleteFile "docs/content/release-notes.md"
    CopyFile "docs/content/" "RELEASE_NOTES.md"
    Rename "docs/content/release-notes.md" "docs/content/RELEASE_NOTES.md"

    DeleteFile "docs/content/license.md"
    CopyFile "docs/content/" "LICENSE.txt"
    Rename "docs/content/license.md" "docs/content/LICENSE.txt"

    generateHelp' true true
)

Target "KeepRunning" (fun _ ->
    use watcher = !! "docs/content/**/*.*" |> WatchChanges (fun changes ->
         generateHelp' true true
    )

    traceImportant "Waiting for help edits. Press any key to stop."

    System.Console.ReadKey() |> ignore

    watcher.Dispose()
)

Target "GenerateDocs" DoNothing

let createIndexFsx lang =
    let content = """(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use
// it to define helpers that you do not want to show in the documentation.
#I "../../../bin"

(**
F# Project Scaffold ({0})
=========================
*)
"""
    let targetDir = "docs/content" </> lang
    let targetFile = targetDir </> "index.fsx"
    ensureDirectory targetDir
    System.IO.File.WriteAllText(targetFile, System.String.Format(content, lang))

Target "AddLangDocs" (fun _ ->
    let args = System.Environment.GetCommandLineArgs()
    if args.Length < 4 then
        failwith "Language not specified."

    args.[3..]
    |> Seq.iter (fun lang ->
        if lang.Length <> 2 && lang.Length <> 3 then
            failwithf "Language must be 2 or 3 characters (ex. 'de', 'fr', 'ja', 'gsw', etc.): %s" lang

        let templateFileName = "template.cshtml"
        let templateDir = "docs/tools/templates"
        let langTemplateDir = templateDir </> lang
        let langTemplateFileName = langTemplateDir </> templateFileName

        if System.IO.File.Exists(langTemplateFileName) then
            failwithf "Documents for specified language '%s' have already been added." lang

        ensureDirectory langTemplateDir
        Copy langTemplateDir [ templateDir </> templateFileName ]

        createIndexFsx lang)
)

// --------------------------------------------------------------------------------------
// Release Scripts

Target "ReleaseDocs" (fun _ ->
    let tempDocsDir = "temp/gh-pages"
    CleanDir tempDocsDir
    Repository.cloneSingleBranch "" (gitHome + "/" + gitName + ".git") "gh-pages" tempDocsDir

    CopyRecursive "docs/output" tempDocsDir true |> tracefn "%A"
    StageAll tempDocsDir
    Git.Commit.Commit tempDocsDir (sprintf "Update generated documentation for version %s" release.NugetVersion)
    Branches.push tempDocsDir
)

#load "paket-files/build/fsharp/FAKE/modules/Octokit/Octokit.fsx"
open Octokit



Target "Watch" (fun _ ->
    use watcher = 
        !! "src/**/*.fs" 
        ++ "tests/**/*.fs"
        ++ "src/**/project.json"
        ++ "tests/**/project.json" 
        |> WatchChanges (fun changes -> 
            tracefn "%A" changes
            runTestSuite()
        )

    System.Console.ReadLine() |> ignore //Needed to keep FAKE from exiting

    watcher.Dispose() // Use to stop the watch from elsewhere, ie another task.
)

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "Build"
  ==> "CopyBinaries"
  =?> ("RunTests", runTests)
  =?> ("CreateDB",resetDB)
//   ==> "GenerateReferenceDocs"
//   ==> "GenerateDocs"
  ==> "All"
  =?> ("ReleaseDocs",isLocalBuild)

// "CleanDocs"
//   ==> "GenerateHelp"
//   ==> "GenerateReferenceDocs"
//   ==> "GenerateDocs"

// "CleanDocs"
//   ==> "GenerateHelpDebug"

// "GenerateHelpDebug"
//   ==> "KeepRunning"
  
RunTargetOrDefault "All"
