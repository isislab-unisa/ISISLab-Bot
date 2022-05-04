import nltk
import pickle
import json
import random
import numpy as np

from flask import Flask, request
from nltk.stem import WordNetLemmatizer
from tensorflow.keras.models import load_model

lemmatizer = WordNetLemmatizer()
model = load_model('helpdesk_model.h5')

intents = json.loads(open('intents.json').read())
words = pickle.load(open('helpdesk_words.pkl','rb'))
classes = pickle.load(open('helpdesk_classes.pkl','rb'))


def clean_up_sentence(sentence):
    sentence_words = nltk.word_tokenize(sentence)
    sentence_words = [lemmatizer.lemmatize(word.lower()) for word in sentence_words]
    return sentence_words

def bow(sentence, words):
    sentence_words = clean_up_sentence(sentence)
    bag = [0]*len(words)  
    for s in sentence_words:
        for i,w in enumerate(words):
            if w == s: 
                bag[i] = 1
    return(np.array(bag))

def predict_class(sentence, model):
    p = bow(sentence, words)
    res = model.predict(np.array([p]))[0]
    ERROR_THRESHOLD = 0.25
    results = [[i,r] for i,r in enumerate(res) if r>ERROR_THRESHOLD]
    results.sort(key=lambda x: x[1], reverse=True)
    return_list = []
    for r in results:
        return_list.append({"intent": classes[r[0]], "probability": str(r[1])})
    return return_list

def getResponse(ints, intents_json):
    tag = ints[0]['intent']
    list_of_intents = intents_json['intents']
    for i in list_of_intents:
        if(i['tag'] == tag):
            result = random.choice(i['responses'])
            break
    return result

def generate_intent(msg):
    ints = predict_class(msg, model)
    res = getResponse(ints, intents)
    return res


app = Flask(__name__)
@app.route('/api/predict', methods=['GET'])
def predict():
    args = request.args
    message = args.get('message')
    
    if message is not None:
        intent = generate_intent(message)
        result = {'Result': 'ok', 'Intent': intent}
    else:
        result = {'Result': 'failed', 'Intent':''}

    return result

if __name__ == '__main__':
    app.run(debug=False)