import { api } from "./client";
import type { UploadedFileDto } from "./types";

type LocalFile = { uri: string; name: string; type: string };

export const fileApi = {
  uploadImage: (file: LocalFile) => api.upload<UploadedFileDto>("/api/files/images", file),
  uploadAudio: (file: LocalFile) => api.upload<UploadedFileDto>("/api/files/audio", file),
  uploadDocument: (file: LocalFile) => api.upload<UploadedFileDto>("/api/files/documents", file),
};
