FROM python:3.7-slim-buster
WORKDIR /app

COPY src/AI/src/* .
COPY ext/models/* .

RUN pip install -r requirements.txt
RUN python -m nltk.downloader punkt
RUN python -m nltk.downloader wordnet

CMD [ "python", "-m", "flask", "run", "--host=0.0.0.0" ]