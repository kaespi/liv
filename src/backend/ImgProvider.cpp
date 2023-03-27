#include "ImgProvider.h"

ImgProvider::ImgProvider(FileSystemWalker* const ptrFileSystemWalker)
    : QQuickImageProvider(QQuickImageProvider::Pixmap), m_ptrFileSystemWalker(ptrFileSystemWalker)
{}

QPixmap ImgProvider::requestPixmap(const QString& imgName, QSize* size, const QSize& /*requestedSize*/)
{
    QString imgFileToLoad{};
    if (imgName.contains('_'))
    {
        auto ixUnderscore = imgName.lastIndexOf('_');
        auto command = imgName.last(imgName.size() - ixUnderscore - 1);
        if (command == "next")
        {
            imgFileToLoad = m_ptrFileSystemWalker->getNextFile();
        }
        else if (command == "prev")
        {
            imgFileToLoad = m_ptrFileSystemWalker->getPrevFile();
        }
        else
        {
            qWarning("Unknown command %s", command.toLatin1().data());
            imgFileToLoad = m_ptrFileSystemWalker->getCurrentFile();
        }
    }
    else
    {
        imgFileToLoad = m_ptrFileSystemWalker->getCurrentFile();
    }

    if (not imgFileToLoad.isEmpty())
    {
        QPixmap pixmap(imgFileToLoad);
        *size = pixmap.size();
        return pixmap;
    }
    else
    {
        // null pixmap (to signal no image available)
        QPixmap pixmap{1, 1};
        *size = QSize(1, 1);
        pixmap.fill(Qt::black);
        return pixmap;
    }
}
