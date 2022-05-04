cd /D "%~dp0"

docker image build -t lab/ai_service -f src/AI/Dockerfile .
pause