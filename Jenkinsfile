pipeline {
  agent any
  stages {
    stage('Build Projects') {
      steps {
        sh 'dotnet build **/MLEM*.csproj'
        sh 'dotnet build **/Demos.csproj'
        sh 'dotnet build **/Demos.DesktopGL.csproj'
      }
    }

  }
}