#pragma once

#include "FileSystemWalker.h"

#include <QQuickImageProvider>

class ImgProvider : public QQuickImageProvider {
public:
  ImgProvider(FileSystemWalker &fileSystemWalker);
  ~ImgProvider() = default;

  QPixmap requestPixmap(const QString &id, QSize *size,
                        const QSize &requestedSize) override;

private:
  FileSystemWalker &m_fileSystemWalker;
};
