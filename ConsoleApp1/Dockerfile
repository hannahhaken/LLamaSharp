FROM --platform=linux/amd64 ghcr.io/chroma-core/chroma:0.3.23

# Fix numpy ABI crash issues
RUN pip install numpy==1.26.4 pandas==1.5.3

EXPOSE 8000

CMD ["uvicorn", "chromadb.app:server", "--host", "0.0.0.0", "--port", "8000"]