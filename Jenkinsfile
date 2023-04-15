pipeline {
  agent none
  stages {
    stage('Cake') {
      agent any
      stages {
        stage('Submodules') {
          steps {
            sh 'git submodule update --init --recursive --force'
          }
        }
        stage('Build') {
          steps {
            sh 'dotnet tool restore'
            // we use xvfb to allow for graphics-dependent tests
            sh 'xvfb-run -a dotnet cake --target Publish --branch ' + env.BRANCH_NAME
          }
        }
        stage('Document') {
          steps {
            sh 'dotnet cake --target Document --branch ' + env.BRANCH_NAME
            stash includes: 'Docs/_site/**', name: 'site'
          }
        }
      }
    }
    stage('Publish Docs') {
      agent { label 'web' }
      when { branch 'release' }
      steps {
        unstash 'site'
        sh 'rm -rf /var/www/MLEM/*'
        sh 'cp Docs/_site/** /var/www/MLEM/ -r'
      }
    }
  }
  post {
    always {
      nunit testResultsPattern: '**/TestResults.xml'
      cobertura coberturaReportFile: '**/coverage.cobertura.xml'
    }
  }
  environment {
    BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
    NUGET = credentials('e1bf7f6c-6047-4f7e-b639-15240a8f8351')
  }
}
