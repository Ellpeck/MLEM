pipeline {
  agent any
  stages {
    stage('Cake Build') {
      steps {
        sh 'chmod +x ./build.sh'
        sh './build.sh -Target=Publish -Branch=' + env.BRANCH_NAME
      }
    }
    stage('Document') {
      when {
        branch 'release' 
      }
      steps {
        sh './build.sh -Target=Document'     
        sh 'cp Docs/_site/** /var/www/MLEM/ -r'   
      }
    }
  }
  environment {
    BAGET = credentials('3db850d0-e6b5-43d5-b607-d180f4eab676')
    NUGET = credentials('e1bf7f6c-6047-4f7e-b639-15240a8f8351')
  }
}
