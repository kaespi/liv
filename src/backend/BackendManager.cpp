#include "BackendManager.h"

BackendManager::BackendManager(const QString &file)
    : m_fileSystemWalker(FileSystemWalker(file)) {
  m_ptrImgProvider = new ImgProvider(m_fileSystemWalker);
}

BackendManager::~BackendManager() {
  if (not m_imgProviderRequested && m_ptrImgProvider) {
    delete m_ptrImgProvider;
  }
}

ImgProvider *BackendManager::requestImgProvider() {
  m_imgProviderRequested = true;
  return m_ptrImgProvider;
}
