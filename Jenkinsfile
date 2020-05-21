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

    stage('Pack and Publish (Master)') {
      when { 
        branch 'master' 
      }
      steps {
        sh 'find . -type f -name \'*.nupkg\' -delete'
        sh '''for i in **/MLEM*.csproj; do
    dotnet pack $i --version-suffix ${BUILD_NUMBER}
done'''
        sh '''for i in **/*.nupkg; do
    dotnet nuget push -s http://localhost:5000/v3/index.json $i -k $BAGET -n true
done'''
        sh '''/opt/docfx/docfx.exe "Docs/docfx.json"
cp Docs/_site /var/www/MLEM/Docs/_site -r'''
      }
    }
    
    stage('Pack and Publish (Release)') {
      when { 
        branch 'release' 
      }
      steps {
        sh 'find . -type f -name \'*.nupkg\' -delete'
        sh '''for i in **/MLEM*.csproj; do
    dotnet pack $i
done'''
        sh '''for i in **/*.nupkg; do
    dotnet nuget push -s https://api.nuget.org/v3/index.json $i -k $NUGET -n true
done'''
        sh '''/opt/docfx/docfx.exe "Docs/docfx.json"
cp Docs/_site /var/www/MLEM/Docs/_site -r'''
      }
    }

  }
  environment {
    BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
    NUGET = credentials('e1bf7f6c-6047-4f7e-b639-15240a8f8351')
  }
}
