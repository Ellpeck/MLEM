pipeline {
  agent any
  stages {
    stage('Build Projects') {
      steps {
        sh '''for i in **/MLEM*.csproj; do
    dotnet build $i
done'''
        sh 'dotnet build **/Demos.csproj'
      }
    }

    stage('Pack') {
      steps {
        sh 'rm **/*.nupkg'
        sh '''for i in **/MLEM*.csproj; do
    dotnet pack $i --version-suffix ${BUILD_NUMBER}
done'''
      }
    }

    stage('Publish') {
      steps {
        sh '''for i in **/*.nupkg; do
    nuget push $i -s https://nuget.ellpeck.de/v3/index.json
done'''
      }
    }

  }
}