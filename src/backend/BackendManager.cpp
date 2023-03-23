#include "BackendManager.h"

BackendManager::BackendManager(const QString &file) {
  m_ptrImgProvider = new ImgProvider(file);
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
