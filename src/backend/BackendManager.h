#pragma once

#include "FileSystemWalker.h"
#include "ImgProvider.h"

class BackendManager {
public:
  BackendManager(const QString &file);
  ~BackendManager();

  ImgProvider *requestImgProvider();

private:
  bool m_imgProviderRequested{false};
  ImgProvider *m_ptrImgProvider{nullptr};
  FileSystemWalker m_fileSystemWalker;
};
