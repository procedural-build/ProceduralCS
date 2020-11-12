pipeline {
  agent jenkins-windows
  stages {

    stage('YAK Build') {
      when {
        branch 'master'
      }
      steps {
        sh 'dir'
        echo '$HOME'
        sh '$HOME\yak.exe version'
        sh '$HOME\yak.exe build'
      }
    }

    stage('YAK Publish') {
      when {
        branch 'master'
      }
      steps {
        sh '$HOME\yak.exe push proceduralcs*'
      }
    }    
  }
  environment {
    SLACK = credentials('slack')
    HOME = '.'
  }
  
  post {
    success {
      slackSend(message: "SUCCESS\nJob: ${env.JOB_NAME} \nBuild ${env.BUILD_DISPLAY_NAME} \n URL: ${env.RUN_DISPLAY_URL}", color: 'good', token: "${SLACK}", baseUrl: 'https://traecker.slack.com/services/hooks/jenkins-ci/', channel: '#jenkins-ci')
    }
    failure {
      slackSend(message: "FAILED\nJob: ${env.JOB_NAME} \nBuild ${env.BUILD_DISPLAY_NAME} \n URL: ${env.RUN_DISPLAY_URL}", color: '#fc070b', token: "${SLACK}", baseUrl: 'https://traecker.slack.com/services/hooks/jenkins-ci/', channel: '#jenkins-ci')
    }

  }
}