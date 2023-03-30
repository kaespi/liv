#pragma once

#include "IFileSystemWalker.h"

#include <QQuickImageProvider>

class ImgProvider : public QQuickImageProvider
{
  public:
    ImgProvider(IFileSystemWalker* const ptrFileSystemWalker);
    ~ImgProvider() = default;

    QPixmap requestPixmap(const QString& imgName, QSize* size, const QSize& /*requestedSize*/) override;

  private:
    IFileSystemWalker* const m_ptrFileSystemWalker{nullptr};
};
