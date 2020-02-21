pipeline {
  agent any
  environment {
      BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
  }
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
        sh 'find . -type f -name \'*.nupkg\' -delete'
        sh '''for i in **/MLEM*.csproj; do
    dotnet pack $i --version-suffix ${BUILD_NUMBER}
done'''
      }
    }

    stage('Publish') {
      steps {
        sh '''for i in **/*.nupkg; do
    dotnet nuget push -s http://localhost:5000/v3/index.json $i -k $BAGET
done'''
      }
    }

  }
}