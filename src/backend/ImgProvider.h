#pragma once

#include "FileSystemWalker.h"

#include <QQuickImageProvider>

class ImgProvider : public QQuickImageProvider {
public:
  ImgProvider(FileSystemWalker *const ptrFileSystemWalker);
  ~ImgProvider() = default;

  QPixmap requestPixmap(const QString &id, QSize *size,
                        const QSize &requestedSize) override;

private:
  FileSystemWalker *const m_ptrFileSystemWalker{nullptr};
};
