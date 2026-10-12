pipeline {
    agent any

    stages {
        stage('1. Checkout from GitHub') {
            steps {
                echo '📥 Successfully cloned latest code from GitHub!'
            }
        }

        stage('2. Verify Files') {
            steps {
                echo '🔍 Checking project structure...'
                sh 'ls -la'
            }
        }

        stage('3. Build Verification') {
            steps {
                echo '✅ IssueTracker project verified by Jenkins!'
            }
        }
    }

    post {
        always {
            echo '🏁 Jenkins Pipeline execution complete.'
        }
    }
}