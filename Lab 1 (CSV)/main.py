from app import *
from csvEngine import CSVEngine

engine = CSVEngine()

app = Application(engine)

app.run()