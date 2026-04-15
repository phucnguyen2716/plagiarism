from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.routers import upload, grading, submissions


@asynccontextmanager
async def lifespan(app: FastAPI):
    # Storage is loaded on module import (app.storage)
    yield


app = FastAPI(title="Automated Tutor Feedback Engine", lifespan=lifespan)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["http://localhost:5173"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(upload.router, prefix="/api")
app.include_router(grading.router, prefix="/api")
app.include_router(submissions.router, prefix="/api")
