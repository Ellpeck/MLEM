pipeline {
  agent any
  stages {
    stage('Build Projects') {
      steps {
        sh '''for i in **/MLEM*.csproj; do
    dotnet build $i
done'''
        sh 'dotnet build **/Demos.csproj'
        sh 'dotnet build **/Demos.DesktopGL.csproj'
      }
    }

  }
}